#!/usr/bin/env bash
#
# Install specific version of Azure Cli in the docker image to run the IaC code
# 

# Update the latest packages and make sure certs, curl, https transport, and related packages are updated.
apt-get update
apt-get install -y ca-certificates curl apt-transport-https lsb-release gnupg

# Download and install the Microsoft signing key.
curl -sL https://packages.microsoft.com/keys/microsoft.asc | \
    gpg --dearmor | \
    tee /etc/apt/trusted.gpg.d/microsoft.asc.gpg > /dev/null

# Add the software repository of the Azure CLI.
AZ_REPO=$(lsb_release -cs)
echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ $AZ_REPO main" | \
    tee /etc/apt/sources.list.d/azure-cli.list


# Update the repository information and install the azure-cli package.
apt-get update
apt-get install python
if [ "$AZURE_CLI_VERSION" = 0 ]; then
    apt-get install azure-cli
else
    apt-get install azure-cli="$AZURE_CLI_VERSION"-1~focal -V
fi
