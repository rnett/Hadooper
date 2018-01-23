# start.sh <machine#> <jarname> <mainclass> <username> <args>
ssh -o StrictHostKeyChecking=no 127x0$1.csc.calpoly.edu "hadooper/.scripts/ready.sh $2"
ssh -o StrictHostKeyChecking=no 127x0$1.csc.calpoly.edu "cd hadooper/$2/java ; ../../.scripts/comprun.sh $2 $3 $4 $5"
mkdir hadooper/$2/output
ssh 127x0$1.csc.calpoly.edu "hadoop fs -copyToLocal hadooper/output hadooper/$2/"
