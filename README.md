# Setting Up & Invoking Serverless Locally

This system is made up of lambda functions deployed to AWS using the [Serverless](https://serverless.com/) framework and a runtime of dotnetcore2.1.

- Install Serverless globally on your machine. The instructions to install serverless can be found [here](https://serverless.com/framework/docs/getting-started/).

You do not need to create a serverless dashboard account to invoke these lambda functions from your local machine.

## Invoking lambda functions locally

- `cd` into `GetDocumentsFromComino` folder.

- Run the `build.sh` file locally on your machine to make sure the project can build on your local machine.

- Run docker on your machine. Instructions to install docker can be found [here](https://docs.docker.com/get-docker/).

- Run `serverless invoke local --function <lambda_function_name>` to invoke the function. `<lambda_function_name>` should be the name of the function as written in the `serverless.yml` file.
