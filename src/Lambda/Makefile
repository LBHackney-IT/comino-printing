include ./.env

.PHONY: invoke-lambda
invoke-lambda:
	./build.sh
	serverless invoke local --stage dev -e COMINO_DB_CONN_STR=${COMINO_DB_CONN_STR} --function fetchdocumentids
