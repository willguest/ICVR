#  ICVR Documentation

This is the entry point for the AI-supported documentation. The files and folders should reflect the code structure, to aid automatic updates and change monitoring, until an autonomous system is in place.


## Writing Documentation

The current strategy aims to be extremely easy to implement and not interfere with the reading of code.  Every significant class should have a corresponding markdown file in the documentation.


### Procedure for Creating AI-Assisted Documentation 

1. Add a summary section to the top of the class definition. Write a short (1-2 sentence) summary. This should be carefully considered because it will also tell the AI what it is about. 
> Note: Using an AI to do this can lead to errors.


2. Give the LLM the following prompt:
```
write documentation for this code. be concise, use Markdown language and include a 'how it works' section at the end. begin the answer with a single inverted comma.
```
> Note: This is somewhat stochastic, but does tend to converge on a decent structure. For better results, provide one of the existing documents as a style example.


3. Once a satisfactory output has been generated, put the reponse into a text file and name it according to the class it documents. Save it using the same folder structure as `Assets/ICVR/Scripts`.
> Note: This will eventually be automated, creating a kind of Wiki within the repository, that is accessable from most IDEs and on the web.


4. Connect the markdown file to the code using the following addition to the end of the XML summary, in the C# script:
```
<para /><see href="https://github.com/willguest/ICVR/tree/develop/Documentation/<Folder>/<ClassName>.md"/>
```





