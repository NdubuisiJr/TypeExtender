# TypeExtender
A light-weight (actually contains one class) library that extends any unsealed type at runtime.

 <div align="center" >
  
 [![forthebadge](https://forthebadge.com/images/badges/made-with-c-sharp.svg)](https://forthebadge.com)
 
 
 [![Actions Status](https://github.com/NdubuisiJr/TypeExtender/workflows/test/badge.svg?style=flat-square)](https://github.com/NdubuisiJr/TypeExtender/actions)
 [![Actions Status](https://github.com/NdubuisiJr/TypeExtender/workflows/Deployment/badge.svg?style=flat-square)](https://github.com/NdubuisiJr/TypeExtender/actions)
 [![nuget](https://img.shields.io/nuget/v/TypeExtender.svg?style=flat-square)](https://www.nuget.org/packages/TypeExtender/)
 [![Nuget](https://img.shields.io/nuget/dt/TypeExtender?style=flat-square)](https://www.nuget.org/packages/TypeExtender/)

<sub>Built by Ndubuisi Jr Chukuigwe</sub>
</div><br>

## Usage
Simply install nuget package and you are ready to zoom off!
### Creating a new class
```
var className = "ClassA";
var typeExtender = new TypeExtender(className);
var returnedType = typeExtender.FetchType();
var obj = Activator.CreateInstance(returnedClass);
```

### Extending an existing class
```
var className = "ClassA";
var baseType = typeof(List<string>);
var typeExtender = new TypeExtender(className, baseType);
var returnedClass = typeExtender.FetchType();
var obj = Activator.CreateInstance(returnedClass);
```

### Adding CustomAttribute to type
```
 var typeExtender = new TypeExtender("ClassA");
 typeExtender.AddAttribute<CustomAAttribute>(new object[] { "Jon Snow" });
```
### Adding Properties to type
There are several overloads on the AddProperty method. Few of them are shown below:
```
var typeExtender = new TypeExtender("ClassA");
typeExtender.AddProperty("IsAdded", typeof(bool));
typeExtender.AddProperty("IsEnabled", typeof(bool), true);
typeExtender.AddProperty<double>("Length");
typeExtender.AddProperty<double>("Width", true);
```
You can add properties with custom attributes
```
 var attributeType = typeof(CustomAAttribute);
 var attributeParams = new object[] { "Jon Snow" };
 var typeExtender = new TypeExtender("ClassA");
 typeExtender.AddProperty("IsAdded", typeof(bool), attributeType, attributeParams);
```
