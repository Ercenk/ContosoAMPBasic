#!/bin/bash

echo "Running deploy.sh"
# Setup error handling
tempfiles=( )
cleanup() {
  rm -f "${tempfiles[@]}"
}
trap cleanup 0

error() {
  local parent_lineno="$1"
  local message="$2"
  local code="${3:-1}"
  if [[ -n "$message" ]] ; then
    echo "Error on or near line ${parent_lineno}: ${message}; exiting with status ${code}"
  else
    echo "Error on or near line ${parent_lineno}; exiting with status ${code}"
  fi

  exit "${code}"
}
trap 'error ${LINENO}' ERR

upload_file(){
  newFile=$1
  newFileToUpload="${newFile//'./'/}"
  uploadDirectory=$2
  ftp_username=$3
  ftp_password=$4
  uploadLocation=$(echo $uploadDirectory/$newFileToUpload)
  echo -e "\n"
  echo "curl -T $newFile -u $ftp_username:$ftp_password $uploadLocation --ftp-create-dirs"
  curl -T "$newFile" -u $ftp_username:$ftp_password "$uploadLocation" --ftp-create-dirs
}

# import variables
. variables.conf

#Build the project 
echo "Checking terraform"
if [[ ! -d ".terraform" ]]; then
    terraform init
fi

terraform apply -auto-approve \
                -var="base_name=$base_name" \
                -var="location=$resource_group_location" 

dotnet restore ./Dashboard/Dashboard.csproj
dotnet build ./Dashboard/Dashboard.csproj -c Release -o ./release
dotnet publish ./Dashboard/Dashboard.csproj -c Release -o ./app

# Deploy the project
resource_group=$(echo $base_name)rg
app_service=$(echo $base_name)AppService
app_service_plan=$(echo $base_name)AppServicePlan
stg_account_name=$(echo $base_name)storage
storage_connection_string=$(az storage account show-connection-string --name $stg_account_name --query connectionString --output tsv)

cat ./app/appsettings.json | sed -i "/OperationsStoreConnectionString/c \"OperationsStoreConnectionString\": \"$storage_connection_string\"" ./app/appsettings.json

#get FTP credentials
ftp_creds=($(az webapp deployment list-publishing-profiles \
  --name $app_service \
  --resource-group $resource_group \
  --query "[?contains(publishMethod, 'FTP')].[publishUrl,userName,userPWD]" \
  --output tsv))

ftpserver=${ftp_creds[0]}
server=${ftpserver#"ftp://"}
ftp_user=${ftp_creds[1]}
ftp_password=${ftp_creds[2]}
echo $ftp_user

pushd app
#build folders
export -f upload_file
find . -type f -exec bash -c "upload_file \"{}\" $ftpserver '$ftp_user' $ftp_password" \;

popd

echo "Restarting $app_service"
az webapp restart -n $app_service -g $resource_group
echo "Restart complete"
