# 38. Use Litmus for Email Testing

Date: 2022-11-14

## Status

Proposed

## Context

We would like to have a way to quickly preview our emails that get sent out to states. Previously we would have to send the email notifications to multiple emails, which could be verified in different email clients.

This works, but the scope is small as the number of email clients we can test in are somewhat limited. Multiple accounts need to be created, different mobile devices, and a variety of desktop apps that need to be installed and verified is a time consuming process.

Litmus makes testing emails in different browsers extremely quick and comprehensive. You can send your email to a litmus test email and verify all browsers at once.
![Litmus Dashboard](https://user-images.githubusercontent.com/100242029/199254427-5bca8458-5e12-44a3-b3f3-4bd578c9aa3d.png)

In addition, Litmus alerts you if there are accessibility issues that come up with your emails. We were able to use this feature to fix a few accessibility issues that we had with our own emails. As we create new emails this feature would be very beneficial.

Other options considered include Email on Acid, but the pricing was similar and Litmus seemed to support more email clients. It's also unclear if Email on Acid would have the accessibility checks that Litmus has.

A common free option for testing emails called Putsmail could only send the email. It does not provide email client previews and wasn't useful for what we were trying to achieve.

## Decision

Propose to use Litmus for email testing. Licensing and actual test workflows TBD. There has also been talk about using something other than emails to notify users in the NAC, such as Push Notifications. All of this needs to be considered before making the decision to purchase a Litmus license.

A Litmus Basic license should suffice, as we would get 1,000 Email Previews / month and unlimited read-only users. If there are around 50 previews generated every time an email is sent, this leaves us with 20 emails we can test manually per month. Litmus Plus would also be a good option if we see a need to test more regularly, but at our current need Basic would suffice.

## Consequences

-  Quickly identify visual or accessibility issues with emails
-  Fees for Litmus licensing increase product cost

## References
* [Litmus](https://www.litmus.com/email-testing/)