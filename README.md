# Troubleshooting and Reporting Errors
At the top until I'm sure it actually works.

I haven't been able to test the Unix scripts on someone else's account, for obvious reasons.  If it doesn't work, make an issue here or email me.

If the scripts don't copy over, they are in the Hadooper/scripts folder in the repository, you can put them in hadooper/.scripts manually.

If ssh isn't working: run `cat /dev/zero | ssh-keygen -f .ssh/hadooper_key -q -N ""` then `cat .ssh/hadooper_key.pub >> .ssh/authorized_keys`.

**_If you make a github issue or email me, attach the Hadooper_log.txt file that will appear next to your executable._**  If there isn't one, there is a 99% chance the error is in your arguments


# Hadooper
Utility script for running hadoop jobs on the CP hadoop server, from windows.  It will upload source and input files to the CP Unix server, compile them, upload the input files to the hadoop server, then run the job and download the output (and compile logs).

Ran like `Hadooper.exe <java folder> <inputfolder> <username> <password> [host#] [-e extra args]`

`<java folder>` is the folder containing your .java files.

`<input folder>` is the folder containing your hadoop input files.  Note that this can be a relative path from `<java folder>`.

Hadooper will copy the hadoop output, as well as log files, into `<input folder>/../output`, aka a folder named output that has the same parent as `<input folder>`.

`<username>` is your CP Unix username

`<password>` is your CP Unix password

Making it work with ssh keys is on the todo list

`[host#]` is the number of the hadoop machine, e.g. 127x0?.  If not specified a random machine (other than 2) is picked.

`[-e extra args]` is the extra arguments to be passed to the hadoop job.  Arguments can be listed normally, but must follow -e flag.



Hadooper will search the java files in <javafolder> for the main method, and will use the name of that file as the driver.


Example: Hadooper.exe "C:\Poly\CSC 369\Lab2\src" "C:\Poly\CSC 369\Lab2\input" uname password -e 5

### On the Unix server
Hadooper creates a hadooper directory, and works out of there.  It will automatically set up ssh so that it can ssh into the 127x0? machines without a password.  It shouldn't affect anything you do on the server.  

It does the same on the hadoop file system.
