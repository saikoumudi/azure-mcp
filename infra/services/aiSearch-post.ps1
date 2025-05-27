param(
    [string] $ResourceGroupName,
    [string] $BaseName
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../eng/common/scripts/common.ps1"

# Get current subscription ID
$subscriptionId = (Get-AzContext).Subscription.Id

$token = Get-AzAccessToken -ResourceUrl https://search.azure.com -AsSecureString | Select-Object -ExpandProperty Token
$uri = "https://$BaseName.search.windows.net"

$apiVersion = "2024-09-01-preview"

$indexDefinition = [ordered]@{
  name = "products"
  fields = @(
    @{ name = "chunk_id"; sortable = $true; key = $true; analyzer = "keyword" }
    @{ name = "parent_id"; filterable = $true }
    @{ name = "chunk" }
    @{ name = "title" }
    @{ name = "header_1" }
    @{ name = "header_2" }
    @{ name = "header_3" }
    @{ name = "text_vector"; type = "Collection(Edm.Single)"; dimensions = 1536; vectorSearchProfile = "products-azureOpenAi-text-profile" }
    @{ name = "category"; searchable = $false; filterable = $true; facetable = $true }
  )
  scoringProfiles = @()
  suggesters = @()
  analyzers = @()
  tokenizers = @()
  tokenFilters = @()
  charFilters = @()
  similarity = @{
    '@odata.type' = "#Microsoft.Azure.Search.BM25Similarity"
  }
  semantic = @{
    defaultConfiguration = "products-semantic-configuration"
    configurations = @(
      @{
        name = "products-semantic-configuration"
        prioritizedFields = @{
          titleField = @{
            fieldName = "title"
          }
          prioritizedContentFields = @(
            @{
              fieldName = "chunk"
            }
          )
          prioritizedKeywordsFields = @()
        }
      }
    )
  }
  vectorSearch = @{
    algorithms = @(
      @{
        name = "products-algorithm"
        kind = "hnsw"
        hnswParameters = @{
          metric = "cosine"
          m = 4
          efConstruction = 400
          efSearch = 500
        }
      }
    )
    profiles = @(
      @{
        name = "products-azureOpenAi-text-profile"
        algorithm = "products-algorithm"
        vectorizer = "products-azureOpenAi-text-vectorizer"
      }
    )
    vectorizers = @(
      @{
        name = "products-azureOpenAi-text-vectorizer"
        kind = "azureOpenAI"
        azureOpenAIParameters = @{
          resourceUri = "https://$BaseName.openai.azure.com"
          deploymentId = "embedding-model"
          modelName = "text-embedding-3-small"
        }
      }
    )
    compressions = @()
  }
}

# Set default values for index fields
foreach($field in $indexDefinition.fields) {
    $field.type ??= "Edm.String"
    $field.stored ??= $true
    $field.retrievable ??= $true
    $field.searchable ??= $true

    $field.filterable ??= $false
    $field.facetable ??= $false
    $field.sortable ??= $false
}

$dataSourceDefinition = @{
  name = "products-datasource"
  type = "azureblob"
  credentials = @{ connectionString = "ResourceId=/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$BaseName;" }
  container = @{ name = "searchdocs" }
}

$skillsetDefinition = @{
  name = "products-skillset"
  description = "Skillset to chunk documents and generate embeddings"
  skills = @(
    @{
        '@odata.type' = "#Microsoft.Skills.Text.SplitSkill"
        name = "#1"
        description = "Split skill to chunk documents"
        context = "/document"
        defaultLanguageCode = "en"
        textSplitMode = "pages"
        maximumPageLength = 2000
        pageOverlapLength = 500
        maximumPagesToTake = 0
        unit = "characters"
        inputs = @(
            @{
                name = "text"
                source = "/document/content"
                inputs = @()
            }
        )
        outputs = @(
            @{
                name = "textItems"
                targetName = "pages"
            }
        )
    }
    @{
        '@odata.type' = "#Microsoft.Skills.Text.AzureOpenAIEmbeddingSkill"
        name = "#2"
        context = "/document/pages/*"
        resourceUri = "https://$BaseName.openai.azure.com"
        deploymentId = "embedding-model"
        dimensions = 1536
        modelName = "text-embedding-3-small"
        inputs = @(
            @{
                name = "text"
                source = "/document/pages/*"
                inputs = @()
            }
        )
        outputs = @(
            @{
                name = "embedding"
                targetName = "text_vector"
            }
        )
    }  )
  indexProjections = @{
    selectors = @(
      @{
        targetIndexName = "products"
        parentKeyFieldName = "parent_id"
        sourceContext = "/document/pages/*"
        mappings = @(
          @{ name = "text_vector"; source = "/document/pages/*/text_vector" }
          @{ name = "chunk"; source = "/document/pages/*" }
          @{ name = "title"; source = "/document/title" }
          @{ name = "header_1"; source = "/document/sections/h1" }
          @{ name = "header_2"; source = "/document/sections/h2" }
          @{ name = "header_3"; source = "/document/sections/h3" }
          @{ name = "category"; source = "/document/category" }
        )
      }
    )
    parameters = @{
      projectionMode = "skipIndexingParentDocuments"
    }
  }
}

$indexerDefinition = @{
  name = "products-indexer"
  dataSourceName = "products-datasource"
  skillsetName = "products-skillset"
  targetIndexName = "products"
  parameters = @{
    configuration = @{
      dataToExtract = "contentAndMetadata"
      parsingMode = "markdown"
      markdownHeaderDepth = "h3"
      markdownParsingSubmode = "oneToMany"
    }
  }
  fieldMappings = @(
    @{
      sourceFieldName = "metadata_storage_name"
      targetFieldName = "title"
    }
  )
  outputFieldMappings = @()
}

Write-Host "Index definition:" -ForegroundColor Yellow
Write-Host "  $(($indexDefinition | ConvertTo-Json -Depth 10).Replace("`n", "`n  "))"

Write-Host "Datasource definition:" -ForegroundColor Yellow
Write-Host "  $(($dataSourceDefinition | ConvertTo-Json -Depth 10).Replace("`n", "`n  "))"

Write-Host "Skillset definition:" -ForegroundColor Yellow
Write-Host "  $(($skillsetDefinition | ConvertTo-Json -Depth 10).Replace("`n", "`n  "))"

Write-Host "Indexer definition:" -ForegroundColor Yellow
Write-Host "  $(($indexerDefinition | ConvertTo-Json -Depth 10).Replace("`n", "`n  "))"

# Create the index
Invoke-RestMethod `
    -Method 'PUT' `
    -Uri "$uri/indexes/$($indexDefinition['name'])?api-version=$apiVersion" `
    -Authentication Bearer `
    -Token $token `
    -ContentType 'application/json' `
    -Body (ConvertTo-Json $indexDefinition -Depth 10)

# Create the datasource
Invoke-RestMethod `
    -Method 'PUT' `
    -Uri "$uri/datasources/$($dataSourceDefinition.name)?api-version=$apiVersion" `
    -Authentication Bearer `
    -Token $token `
    -ContentType 'application/json' `
    -Body (ConvertTo-Json $dataSourceDefinition -Depth 10)

# Create the skillset
Invoke-RestMethod `
    -Method 'PUT' `
    -Uri "$uri/skillsets/$($skillsetDefinition.name)?api-version=$apiVersion" `
    -Authentication Bearer `
    -Token $token `
    -ContentType 'application/json' `
    -Body (ConvertTo-Json $skillsetDefinition -Depth 10)

# Create the indexer
Invoke-RestMethod `
    -Method 'PUT' `
    -Uri "$uri/indexers/$($indexerDefinition.name)?api-version=$apiVersion" `
    -Authentication Bearer `
    -Token $token `
    -ContentType 'application/json' `
    -Body (ConvertTo-Json $indexerDefinition -Depth 10)

# Upload sample files
$context = New-AzStorageContext -StorageAccountName $BaseName -UseConnectedAccount
$container = 'searchdocs'
$categories = @('A', 'B', 'C')
Write-Host "Uploading sample files to blob storage: $BaseName/$container" -ForegroundColor Yellow
$files = Get-ChildItem -Path "$PSScriptRoot/../samples" -Filter '*.md'
$i = 0;
foreach ($file in $files) {
    $category = $categories[$i++ % $categories.Count]
    Write-Host "  $($file.Name)`: { category: $category }" -ForegroundColor Yellow
    Set-AzStorageBlobContent -File $file.FullName -Container $container -Blob $file.Name -Metadata @{ category = $category } -Context $context -Force -ProgressAction SilentlyContinue | Out-Null
}
