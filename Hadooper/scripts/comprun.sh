# compile.sh <jarname> <mainclass> <username> <args>

export PATH

export JAVA_HOME=/usr/jdk64/jdk1.8.0_77
export PATH=${JAVA_HOME}/bin:${PATH}
export HADOOP_CLASSPATH=${JAVA_HOME}/lib/tools.jar
export SPARK_MAJOR_VERSION=2


rm ../$1_compile.out
rm ../$1_hadoop.out

pwd

hadoop com.sun.tools.javac.Main *.java > ../$1_compile.out 2>&1
printf "\n" >> ../$1_compile.out
jar cvf $1.jar *.class >> ../$1_compile.out 2>&1
hadoop jar $1.jar $2 $4 /user/$3/hadooper/input /user/$3/hadooper/output |& tee ../$1_hadoop.out
