# Configure the Microsoft Azure Provider
provider "azurerm" {
    version         = "=2.0.0"
    features{}
}

# Create a resource group if it doesn’t exist
resource "azurerm_resource_group" "resourceGroup" {
    name     = "${var.base_name}rg"
    location = var.location

    tags = {
        environment = "${var.base_name} Marketplace Portal"
    }
}

resource "azurerm_app_service_plan" "appServicePlan" {
  name                = "${var.base_name}AppServicePlan"
  location            = azurerm_resource_group.resourceGroup.location
  resource_group_name = azurerm_resource_group.resourceGroup.name
  kind                = "linux"
  reserved            = true
  sku {
    tier = "Basic"
    size = "B1"
  }

  tags = {
      environment = "${var.base_name} Marketplace Portal"
  }
}

resource "azurerm_app_service" "appService" {
  name                = "${var.base_name}AppService"
  location            = azurerm_resource_group.resourceGroup.location
  resource_group_name = azurerm_resource_group.resourceGroup.name
  app_service_plan_id = azurerm_app_service_plan.appServicePlan.id
  https_only          = true

  site_config {
    linux_fx_version  = "DOTNETCORE|3.0"
  }

  tags = {
      environment = "${var.base_name} Marketplace Portal"
  }
}

resource "azurerm_storage_account" "subscribers" {
    name                        = "${var.base_name}storage"
    resource_group_name         = azurerm_resource_group.resourceGroup.name
    location                    = azurerm_resource_group.resourceGroup.location
    account_tier                = "Standard"
    account_replication_type    = "LRS"
    account_kind                = "StorageV2"

    tags = {
        environment = "${var.base_name} Marketplace Portal"
    }
}
