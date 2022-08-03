# [logAxe](../README.md)

# Filters

logAxe has the following fitler functionality
* Filters strings
    * Include : Include keywords
    * Exclude : Exclude keywords
* Filter log type
    * Filter all log files based on 
        ERROR, TRACE, INFO, WARNING
    * We can filter one or more groups.

# future
* need to add DEBUG.

Discussion on the tag filters.

Tag, is nothing but a summation of threadId, processId, cateogry, module etc that is not yet defined.

Should we make it as a json ? like { procId: <value>, threadId: <value>}