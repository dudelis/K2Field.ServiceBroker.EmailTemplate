[<img src="https://k2-konstantin.visualstudio.com/_apis/public/build/definitions/dd460ed5-13c0-4ae1-a98f-a97b6955e631/3/badge"/>](https://k2-konstantin.visualstudio.com/EmailTemplate/_build/index?definitionId=3)

# Email Template ServiceBroker

## Introduction

The current Service Broker provides functionality to enable dynamic Email Templating inside K2 Worklfows. The idea of the Broker is to change Email/Task Notifications inside the workflow without the necessity to redeploy them. There are 2 options how the placeholders in the Email Subject/Body can be replaced with values at runtime:
- via static values, mapped during the workflow design;
- via dynamic list of placeholders/parameters, obtained from the mapped SmartObject, accompanied by the ADO.NET queries, which are carried out at runtime.

For more details and detailed instruction on how to configure it, please, refer to the documentation below.

## User Manual
Please, refer to the following links:
- [User Manual](https://github.com/dudelis/K2Field.ServiceBroker.EmailTemplate/wiki/User-Manual)
- [How to configure Email templates in a Workflow](https://github.com/dudelis/K2Field.ServiceBroker.EmailTemplate/wiki/How-to-configure-Email-Template-in-the-Workflow)
- [How to create a new Email Template](https://github.com/dudelis/K2Field.ServiceBroker.EmailTemplate/wiki/How-to-create-a-new-Email-template)

## Disclaimer
The code is provided as is. If you want to contribute, please, read the [Contribution Guidelines](CONTRIBUTION.md)

## License
This software is MIT. See [License](LICENSE)
