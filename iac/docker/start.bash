#!/usr/bin/env bash
#
# Script used by docker to start the provisioning of resources 
# as part of the infraestruture as code process
#
azure_env=$1
az login
cd /piipan/iac || exit
echo "Starting create-resources.bash on ${azure_env} enviroment"
./create-resources.bash "$azure_env"