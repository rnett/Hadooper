# Hadooper
Utility script for running hadoop jobs on the CP hadoop server, from windows.

Ran like `Hadooper.exe <java folder> <inputfolder> <username> <password> [host#] [-e extra args]`

`<java folder>` is the folder containing your .java files.

`<input folder>` is the folder containing your hadoop input files.

Hadooper will copy the hadoop output, as well as log files, into <input folder>/../output, aka a folder named output that has the same parent as <input folder>.

`<username>` is your CP Unix username

`<password>` is your CP Unix password

Making it work with ssh keys is on the todo list

`[host#]` is the number of the hadoop machine, e.g. 127x0?.  If not specified a random machine (other than 2) is picked.

`[-e extra args]` is the extra arguments to be passed to the hadoop job.  Arguments can be listed normally, but must follow -e flag.



Hadooper will search the java files in <javafolder> for the main method, and will use the name of that file as the driver.


Example: Hadooper.exe "C:\Poly\CSC 369\Lab2\src" "C:\Poly\CSC 369\Lab2\input" uname password -e 5

## On the Unix server
Hadooper creates a hadoop directory, and works out of there.  It will automatically set up ssh so that it can ssh into the 127x0? machines without a password.

### Troubleshooting
I haven't been able to test the Unix scripts on someone else's account, for obvious reasons.  If it doesn't work, make an issue here or email me.

If the scripts don't copy over, they are in the Hadooper/scripts folder in the repository, you can put them in hadooper/.scripts manually.

If ssh isn't working: run `cat /dev/zero | ssh-keygen -f .ssh/hadooper_key -q -N ""` then `cat .ssh/hadooper_key.pub >> .ssh/authorized_keys`.
