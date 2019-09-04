Upgrade library steps:

1. Update `build.cake` file
```
var VERSION = "0.6.1";
var NUGET_SUFIX = ".0";
```

2. Run command
```
sh build.sh --settings_skipverification=true
```