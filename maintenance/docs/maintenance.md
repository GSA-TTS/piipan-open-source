# Maintenance Project


## Project Summary

A webapp to be published to a maintenance deployment slot in the query tool and dashboard app services temporarily when doing a new deploy.

This web app routes all endpoints with the same base url to the same down for maintenance page. 

## Deployment Slots

To navigate to the deployment slots in azure go to the respective app service, then to deployment slots under the deployment section. 

## Changing routing of the deployment slots

When the maintenance project should be getting all traffic to the app navigate to the deployment slots section of the web service and change the `TRAFFIC %` number from 0 to 100 
for the maintenance slot. 

This will automatically change the default production app to 0%. After a brief delay Azure will now route all traffic to the maintenance slot. 

## IAC Information

In the IAC code the deployment slots are made in the resources array of the web apps. This falls under the query-tool-app.json file and the dashboard-app.json file.

The name of the deployment slot is set as a parameter in the resepective bash script when calling the arm template. 
