# Onboarding a tenant
> ⚠️ This documentation is for Piipan developers only

1. Create a service principal for tenant storage account

## Create a service principal for tenant storage account

In order for a user to upload a csv, a service principal must be created for the specified storage account. This script will generate a service principal and output the credentials to be provided to the user.

From the top-level piipan directory:

```
$ cd iac
$ ./create-state-storage-service-principal.bash [env] [storage account name]
```

Usage example:
```
$ ./create-state-storage-service-principal.bash tts/dev my-storage-name
```

Note that running the script repeatedly will re-generate the credentials.

Once the credentials are generated, securely share them with the user.
