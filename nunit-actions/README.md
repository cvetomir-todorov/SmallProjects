# Introduction

Provides a skeleton example (no internal implementation) how [NUnit actions](https://docs.nunit.org/articles/nunit/extending-nunit/Action-Attributes.html) can be utilized to:

## Setup and tear down test data in data storages

* `SqlServer` actions
  - One for importing data into a database from a single file - can be applied multiple times
  - One for cleaning it up - can be applied once only
  - Can be applied to both test/suite
* `DynamoDb` action
  - A single action for importing data into DynamoDB and cleaning it up - can be applied once only
  - Can be applied to both test/suite
* `Sqs` action
  - One for importing serialized messages - can be applied multiple times
  - One for cleaning it up - can be applied once only
  - Can be applied to both test/suite but targets a test always

## Start and stop environment

* `UseKubernetes` action
  - Starts and stops a Kubernetes environment using a yaml file - can be applied multiple times
  - Can be applied on suite level only and targets the whole suite
