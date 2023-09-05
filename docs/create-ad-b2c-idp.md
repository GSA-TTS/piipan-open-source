# Create & Manage Azure AD B2C IDP

## Background
We use an [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview) tenant as our Open ID Connect identity provider in a sandbox environment.

## Setup

#### Create the tenant
1. From the Azure Portal (primary tenant), select `Create a resource`
2. Search for and select `Azure Active Directory B2C`
   1. Click `Create`
   2. Click `Create a new Azure AD B2C Tenant`
   3. Fill in tenant naming details and click `Create`

#### Create a user flow
1. From the top navigation bar, open the `Directory + subscription` pane and select the newly created tenant
2. From within the B2C tenant, search for and select `Azure AD B2C`
3. Under `Policies`, select the `User flows` blade
   1. Click `New user flow`
   2. Select `Sign in` and click `Create`
   3. Enter a name for the flow (e.g., "B2C-1_SI")
   4. Select `Email signin` under `Local accounts`
   5. Under `Application claims`, select:
      - `Display name`
      - `Identity provider`
      - `User's Object ID`
   1. Click `Create`
   
#### Register an application
1. From within the B2C tenant, search for and select `Azure AD B2C`
2. Open the `App registrations` blade
    1. Click `New registration`
    2. Enter the name to match that of the application object in the main tenant
    3. Under `Supported account types`, select the third option (“any identity provider”)
    4. Set the redirect URI to the base URI of the application, suffixed with `/.auth/login/aad/callback`
    5. Click `Register`

#### Add an administrative user
1. From within the B2C tenant, search for and select `Azure AD B2C`
2. Open the `Users` blade
   1. Click `New user`
   2. Select `Invite user`
   3. Enter the email address of the user to invite
   4. Click `Invite`
3. Once the user has accepted the invitation:
   1. Return to the `Users` blade
   2. Click on the new user
   3. Click `Edit`
   4. In the `Profile` blade, under `User type`, select `Member`
   5. In the `Assigned roles` blade, click `Add assignments`
   6. Search for and check `Global administrator`, click `Add`

#### Add an application user
1. From within the B2C tenant, search for and select `Azure AD B2C`
2. Open the `Users` blade
   1. Click `New user`
   2. Select `Create Azure AD B2C user`
   3. Under `Sign in method`, select `Email`
   4. Enter the email address of the new user
      - NOTE: if this user already has an administrative account with the same email address, some email providers allow for aliases
   5. Set a temporary password for the new user
   6. Click `Create`
   7. Open the profile editing user flow and sign in as the new user. Under email, enter the same email address that was used for sign in and click continue.
      - You should be redirected to [jwt.ms](jwt.ms) where, under `Decoded Token`, you should see a claim with a type of `extension_EmailAddress`.

#### Set or update existing user attributes (claims)
1. From within the B2C tenant, search for and select `Azure AD B2C`
2. Open the `User flows` blade
   1. From the list of user flows, select `B2C_1_pe` (`pe` in this case stands for profile edit)
   2. Click `Run user flow`
   3. Under `Application`, select either the dashboard or querytool
   4. Under `Reply URL`, select `https://jwt.ms`
   5. Click `Run user flow`
   6. The profile editor will open in a new tab. Once you sign in, you will be able to update the attributes for the user, and the associated claims will reflect those updates on the next sign in.

#### Add a new user attribute (claim)
1. From within the B2C tenant, search for and select `Azure AD B2C`
2. Open the `User attributes` blade
   1. Click `Add`
   2. Enter the name and data type of the attribute, and click `Create`
3. Click the `User flows` blade
   1. Select `B2C_1_si` (`si` in this case stands for sign in)
   2. Click `Application claims`
   3. Find the attribute you just added in the list and check the box next to it
   4. Click save
4. Repeat step 3 for the `B2C_1_pe` user flow
5. Follow the [instructions for updating user attribute values](#set-or-update-existing-user-attributes-claims)