include ./.env

.PHONY: invoke-lambda
invoke-lambda:
	./build.sh
	serverless invoke local --stage dev -e COMINO_DB_CONN_STR=${COMINO_DB_CONN_STR} --function fetchdocumentids

.PHONY: run-local-dynamo-db
run-local-dynamo-db:
	sls dynamodb install
	sls dynamodb start

.PHONY: stop-local-db
stop-local-db:
	sls dynamodb remove


