# Comino Printing via Notify

## Setting Up & Invoking Serverless Locally

This system is made up of lambda functions deployed to AWS using the [Serverless](https://serverless.com/) framework and a runtime of dotnetcore2.1.

- Install Serverless globally on your machine. The instructions to install serverless can be found [here](https://serverless.com/framework/docs/getting-started/).

You do not need to create a serverless dashboard account to invoke these lambda functions from your local machine.

## Invoking lambda functions locally

1. Make sure docker is running your machine. Instructions to install docker can be found [here](https://docs.docker.com/get-docker/).
2. Navigate into the Lambda subdirectory: `cd src/Lambda`
3. Run `./build.sh`to make sure the project can build on your local machine.
4. Run `serverless invoke local --function <lambda_function_name>` to invoke a function. `<lambda_function_name>` should be the name of the function as written in the `serverless.yml` file.

## Running Local Dynamo DB

1. Ensure Java is installed `java --version` should return a version number. If not run `brew cask install java`
2. Run make run-local-dynamo-db
3. Go to [here](http://localhost:8001/)

## Changing the Document Settings

The documents that are collected from BatchPrint are defined by category and description in the `DOCUMENT_CONFIG` environment variable. You should see a default value in the `.env.example` file in this project.

To update this value for staging and production, the following command may be useful to avoid unpredictable results from the serialised JSON:

```
aws ssm put-parameter --name "/hn-comino-printing/[stage]/DOCUMENT_CONFIG" --value "{\"Categories\":[\"Benefits/Out-Going\"],\"Descriptions\":[\"Income Verification Document\"]}" --type String --overwrite
```

Replace [stage] above with either `staging` or `production`.

## Sequence Diagram

This repo contains a UML sequence diagram, which can be updated as follows:

1. Run `docker pull think/plantuml` to download [this useful PlantUML image](https://hub.docker.com/r/think/plantuml/)
2. Edit `./sequenceDiagram/source.uml` as needed
3. Run `cat sequenceDiagram/source.uml | docker run --rm -i think/plantuml -tpng > sequenceDiagram/source.png` to output
