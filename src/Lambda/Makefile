include ./.env

.PHONY: invoke-lambda
invoke-lambda:
	./build.sh
	serverless invoke local --stage dev -e COMINO_DB_CONN_STR=${COMINO_DB_CONN_STR} --function fetchdocumentids

.PHONY: run-local-dynamo-db
run-local-dynamo-db:
	sls -s test dynamodb start & DYNAMO_ENDPOINT=http://localhost:8000 dynamodb-admin

.PHONY: stop-local-db
stop-local-db:
	sls dynamodb remove
 

