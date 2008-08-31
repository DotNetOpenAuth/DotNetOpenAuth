This is the future home of YOURLIBNAME.

To customize it for a library:
1. Find & Replace in Files with case sensitive search: 
	YOURLIBNAME -> YourLibrary
2. Do a dir /s *YOURLIBNAME* in the root of the project and rename all files/directories to *YourLibrary*.
	 dir -rec . *YOURLIBNAME* |% { ren $_.fullname $_.name.replace("YOURLIBNAME", "YourLibrary") }