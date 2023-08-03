# ICVR: Internet Computer Virtual Reality 

## Intro 

ICVR the name we give to the collection of tools, assets and workflows that go together to enable the delivery of an on-chain immersive experience. It is the generalised solution that grew out of the iterative development of four unique environments.

It is an open-source, extendable toolkit that offers creative freedom and a method to self-host your VR experience on the blockchain. It aims to lower the technical barrier to the construction of this type of VR framework, running on the Internet Computer's distributed server network.

Here, we are promoting a development pipeline, rather than a platform. We are advocates for a particular development path, one that we ourselves are taking, and we invite you to join us.

## What is this Repo?

This is the first version of the open-source framework. It contains a central test scene, containing only a floor and a network connector.

It is currently provided "as is" and the code is licensed under the [Mozilla Public License 2.0](https://www.mozilla.org/en-US/MPL/2.0/FAQ/)

## How to make an ICVR Experience?

- Using Unity [2020.3 LTS](unityhub://2020.3.48f1/b805b124c6b7), make a URP project.
- Import the ICVR_Core and, if desired, the ICVR_DLC unity packages.
- Follow the configuration tutorial, which is currently designed for a Unity user with 1-2 years experience, which covers:
    * Project setting import / configuration    
    * Build    
    * Start with sample app (blockchain) canister    
    * Modify and deploy canister.    
- Keep your canister topped up with cycles.




## Writing Documentation

>[Current Documentation](https://github.com/willguest/ICVR/tree/develop/Documentation)

The current strategy aims to be extremely easy to implement and not interfere with the reading of code.  Every significant class should have a corresponding markdown file in the documentation.


### Procedure for Creating AI-Assisted Documentation 

1. Add a summary section to the top of the class definition. Write a short (1-2 sentence) summary. This should be carefully considered because it will also tell the AI what it is about. 
> Note: Using an AI to do with can lead to errors.


2. Give the LLM the following prompt:
```
write documentation for this code. be concise, use Markdown language and include a 'how it works' section at the end. begin the answer with a single inverted comma.
```
> Note: This is somewhat stochastic, but does tend to converge on a decent structure.


3. Once a satisfactory output has been generated, put the reponse into a text file and name it according to the class it documents. Save it using the same folder structure as `Assets/ICVR/Scripts`.
> Note: This will eventually be automated, creating a kind of Wiki within the repository, that is accessable from most IDEs and on the web.


4. Connect the markdown file to the code using the following addition to the end of the XML summary, in the C# script:
```
<para /><see href="https://github.com/willguest/ICVR/tree/develop/Documentation/<Folder>/<ClassName>.md"/>
```




