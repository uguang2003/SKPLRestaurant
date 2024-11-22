This folder contains a subset of the Unity Standard Assets files. This subset has been heavily modified to act as a reasonable baseline player controller with related functionality for input, etc.

The filenames and namespaces associated with these files have been altered to avoid namespace conflicts when users of the First Person Exploration Kit asset import a new version of 
the Unity Standard Assets package. For example, a common issue arises when users import this asset and Unity Standard Assets which causes the references to MouseLook 
to be confussed by the compiler, resulting this asset package breaking. Renaming the namespaces, along with the other changes, will help prevent future confusion 
of this nature.