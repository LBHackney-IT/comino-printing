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