include ./.env

.PHONY: invoke-fetch-documents-lambda
invoke-fetch-documents-lambda:
	cd src/Lambda && ./build.sh
	serverless invoke local --stage dev -e COMINO_DB_CONN_STR=${COMINO_DB_CONN_STR} --function fetch-document-ids

.PHONY: invoke-listen-to-sqs-lambda
invoke-listen-to-sqs-lambda:
	cd src/Lambda && ./build.sh
	serverless invoke local --stage dev -e COMINO_DB_CONN_STR=${COMINO_DB_CONN_STR} --function listen-for-sqs-events

.PHONY: run-local-dynamo-db
run-local-dynamo-db:
	sls -s test dynamodb start & DYNAMO_ENDPOINT=http://localhost:8000 dynamodb-admin

.PHONY: stop-local-db
stop-local-db:
	sls dynamodb remove

.PHONY: run-circle
run-circle:
	-docker kill test-dynamodb
	-docker rm test-dynamodb
	circleci config process .circleci/config.yml > process.yml
	circleci local execute -c process.yml --job build 

.PHONY: remove-test-db-docker
remove-test-db-docker:
	-docker kill test-dynamodb
	-docker rm test-dynamodb

.PHONY: deploy-ui
deploy-ui: build-ui
	sls client deploy --no-confirm --aws-profile hackney --stage ${STAGE}

.PHONY: deploy-lambda
deploy-lambda: build-lambda
	sls deploy --aws-profile hackney --stage ${STAGE}

.PHONY: deploy
deploy: deploy-lambda deploy-ui

.PHONY: build-ui
build-ui:
	cd comino-print-ui && npm install && npm run build

.PHONY: build-lambda
build-lambda:
	cd src/Lambda && ./build.sh
