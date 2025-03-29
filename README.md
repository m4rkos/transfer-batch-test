# Transfer batch processing

Given a CSV file with the amount of outbound transfers for each account for a given day, with the following structure:
<Account_ID>,<Transfer_ID>,<Total_Transfer_Amount>

Implement a console application that will calculate the total commissions that should be charged for each account on a given day, with the following rules:
* Accounts should be charged by 10% of the total value on every transfer
* The transfer with the highest value of the day will not be subject to commission

The program must produce an output to the console formatted as follows:
<Account_ID>,<Total_Commission>

## Command line interface
The program must run from the command line with the following arguments
```
TransferBatch <Path_to_transfers_file>
```

## Example
Given the input `transfers.csv` file:
```
A10,T1000,100.00
A11,T1001,100.00
A10,T1002,200.00
A10,T1003,300.00
```

The result should be:
```
$ TransferBatch transfers.csv
A10,30
A11,10
```

## Deliverable
We expect you to deliver a zip file containing the code that implements the solution for this problem.
Plese provide clear instructions on how to build the application.

## Programming languages
We accept solutions implemented in one of the following programming languages:
* C#
* Java
* Python
* GoLang

---

## To run unit test
Just go inside trafer-batch-test and type `dotnet test`

> Author: Marcos Eduardo