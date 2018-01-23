#ready.sh <jarname>

hadoop fs -mkdir hadooper
hadoop fs -rm -r hadooper/output
hadoop fs -rm -r hadooper/input
hadoop fs -mkdir hadooper/input
hadoop fs -copyFromLocal hadooper/$1/input/* hadooper/input
