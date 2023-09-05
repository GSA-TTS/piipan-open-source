#!/usr/bin/env bash
#
# Configures Microsoft Defender and assigns the CIS Microsoft Azure
# Foundations Benchmark 1.3.0 to the subscription. Also configures policies
#
# Assumes the script is executed by an Azure user with the Owner/Contributor role for the subscription.
#
# usage: configure-defender-and-policies.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

set_constants () {
  # The policy "name" is the UUID of the set-definition
  AZURE_DEFENDER_EXPORT_EVENT_HUB_POLICY_SET_DEFINITION_ID="cdfcce10-4578-4ecd-9703-530938e4abcb"
  AZURE_DEFENDER_EXPORT_LOG_ANALYTICS_POLICY_SET_DEFINITION_ID="ffb6f416-7bd2-4488-8828-56585fef2be9"
  # https://docs.microsoft.com/en-us/azure/governance/policy/samples/cis-azure-1-3-0
  # https://docs.microsoft.com/en-us/azure/governance/policy/samples/gov-cis-azure-1-3-0
  # While the policy set definition is the same, it is enforced differently
  # based on commerical or government Azure cloud
  CIS_POLICY_SET_DEFINITION_ID="612b5213-9160-4969-8578-1518bd2a000c"

  set_defaults
}

assign_policy () {
  policy=$1
  policy_set_definition_id=$2
  params=$3

  echo "Assigning Policy - ${policy}"
  az policy assignment create \
    --name "${policy}" \
    --location "${LOCATION}" \
    --policy "${policy_set_definition_id}" \
    --identity-scope "/subscriptions/${SUBSCRIPTION_ID}" \
    --mi-system-assigned \
    --params "${params}"
}

create_non_compliance_message () {
  policy=$1

  echo "Creating Non-Compliance Message - ${policy}"
  az policy assignment non-compliance-message create \
    --message "${policy} is non-compliant" \
    --name "${policy}"
}

create_remediation () {
  policy=$1

  echo "Creating Remediation - ${policy}"
  az policy remediation create \
    --name "${policy}" \
    --policy-assignment "${policy}"
}

configure_policy_resources () {
  policy=$1
  policy_set_definition_id=$2
  params=$3

  assignment=$(\
    az policy assignment show \
      --name "${policy}" \
      --output tsv) || assignment=""
  if [ -z "${assignment}" ]; then
    assign_policy "${policy}" "${policy_set_definition_id}" "${params}"
    create_non_compliance_message "${policy}"
    create_remediation "${policy}"
  fi
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/../env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac-common.bash
  verify_cloud
  set_constants

  echo "Configure Microsoft Defender for Cloud"
  az deployment sub create \
    --name "defender-${ENV}" \
    --location "${LOCATION}" \
    --template-file "$(dirname "$0")"/../arm-templates/security/defender.json \
    --parameters \
      cloudName="${CLOUD_NAME}" \
      additionalEmailAddresses="${SECURITY_ADDITIONAL_EMAILS}" \
      minimalSeverity="${SECURITY_MINIMAL_SEVERITY}" \
      notificationRoles="${SECURITY_NOTIFICATION_ROLES}"

  echo "Configure Policy Resources - ${AZURE_CIS_POLICY}"
  assignment=$(\
    az policy assignment show \
      --name "${AZURE_CIS_POLICY}" \
      --output tsv) || assignment=""
  if [ -z "${assignment}" ]; then
    echo "Assigning Policy - ${AZURE_CIS_POLICY}"
    az policy assignment create \
      --name "${AZURE_CIS_POLICY}" \
      --location "${LOCATION}" \
      --policy-set-definition "${CIS_POLICY_SET_DEFINITION_ID}" \
      --identity-scope "/subscriptions/${SUBSCRIPTION_ID}" \
      --mi-system-assigned
    create_non_compliance_message "${AZURE_CIS_POLICY}"
    create_remediation "${AZURE_CIS_POLICY}"
  fi

  echo "Configure Policy Resources - ${AZURE_DEFENDER_EXPORT_EVENT_HUB_POLICY}"
  event_hub_details="${SUBSCRIPTION_ID}/${EVENT_HUB_NAMESPACE}/${EVENT_HUB_NAME}/${EVENT_HUB_AUTH_RULE}"
  policy_parameters="{ 'resourceGroupName': { 'value': '${SECURITY_RESOURCE_GROUP}' }, 'resourceGroupLocation': { 'value': '${LOCATION}' }, 'eventHubDetails': { 'value': '${event_hub_details}' } }"
  configure_policy_resources "${AZURE_DEFENDER_EXPORT_EVENT_HUB_POLICY}" "${AZURE_DEFENDER_EXPORT_EVENT_HUB_POLICY_SET_DEFINITION_ID}" "${policy_parameters}"

  echo "Configure Policy Resources - ${AZURE_DEFENDER_EXPORT_LOG_ANALYTICS_POLICY}"
  policy_parameters="{ 'resourceGroupName': { 'value': '${SECURITY_RESOURCE_GROUP}' }, 'resourceGroupLocation': { 'value': '${LOCATION}' }, 'workspaceResourceId': { 'value': '${LOG_ANALYTICS_WORKSPACE_ID}' } }"
  configure_policy_resources "${AZURE_DEFENDER_EXPORT_LOG_ANALYTICS_POLICY}" "${AZURE_DEFENDER_EXPORT_LOG_ANALYTICS_POLICY_SET_DEFINITION_ID}" "${policy_parameters}"

  script_completed
}

main "$@"
