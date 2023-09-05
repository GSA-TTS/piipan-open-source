#!/usr/bin/env bash
#
# Sets the Payload encryption key values in Azure Key Vault (as secrets) for encrypting
# and decrypting bulk upload files. These keys will allow Azure API Management
# to encrypt bulk upload files and allow Azure functions to decrypt them
# when processing.
#
# Assumes an Azure user with the Global Administrator role has signed in
# with the Azure CLI, the infrastructure, established by create-resources.bash
# is in place, particularly, the Azure Key Vault instance.
#
# usage: configure-payload-keys.bash <azure-env>
# TODO: Rotation of Key Vault Secrets

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit
# shellcheck source=./tools/build-common.bash
source "$(dirname "$0")"/../tools/build-common.bash || exit

main () {
    local azure_env=$1
    # shellcheck source=./iac/env/tts/dev.bash
    source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
    # shellcheck source=./iac/iac-common.bash
    source "$(dirname "$0")"/../iac/iac-common.bash

    create_kv_secret () {
      secretName=$1
      secretValue=$2
      secret=$(\
        az keyvault secret show \
          --name "${secretName}" \
          --vault-name "${VAULT_NAME}" \
          --query id \
          --output tsv \
          || echo "")

      if [ -z "${secret}" ]; then
        echo "Setting ${secretName} in Key Vault"
        az deployment group create \
        --name "${VAULT_NAME}-${secretName}" \
        --resource-group "${RESOURCE_GROUP}" \
        --template-file ./arm-templates/key-vault-secret.json \
        --parameters \
          keyVaultName="${VAULT_NAME}" \
          resourceTags="${RESOURCE_TAGS}" \
          secretName="${secretName}" \
          secretValue="${secretValue}"
      else
        echo "Not setting ${secretName} in Key Vault, secret exists and not rotating"
      fi
    }

    echo "Generate Column Encryption Key"
    columnKey=$(openssl enc -aes-256-cbc -k secret -P -md sha512 -pbkdf2 | sed -n 's/^key=\(.*\)/\1/p')
    BASE64COLUMNKEY=$(python ../tools/hexToBase64Converter.py "${columnKey}")
    create_kv_secret "${COLUMN_ENCRYPT_KEY_KV}" "${BASE64COLUMNKEY}"

    echo "Generate Payload Encryption Keys"
    payloadKey=$(openssl enc -aes-256-cbc -k secret -P -md sha512 -pbkdf2 | sed -n 's/^key=\(.*\)/\1/p')
    BASE64UPLOADKEY=$(python ../tools/hexToBase64Converter.py "${payloadKey}")
    shaOfPayloadKey=$(python ../tools/sha256Calculator.py "${payloadKey}")
    BASE64UPLOADSHAKEY=$(python ../tools/hexToBase64Converter.py "${shaOfPayloadKey}")
    create_kv_secret "${UPLOAD_ENCRYPT_KEY_KV}" "${BASE64UPLOADKEY}"
    create_kv_secret "${UPLOAD_ENCRYPT_KEY_SHA_KV}" "${BASE64UPLOADSHAKEY}"
}

main "$@"
