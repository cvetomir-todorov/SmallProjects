The format of the data files is binary.
The structure is as follows:

- 1 int for sensor count
- foreach sensor:
  - 1 byte for device ID
  - 1 byte for average
  - 1 int for sensor values count
  - foreach sensor value:
    - 1 byte for the value itself

Data persistance classes are interfaced as async but are implemented using the old sync BinaryWriter since it turned out .NET hasn't implemented them yet.
Options for fixing this include:

1 - custom in-house implementation using the FileStream's async methods and custom buffering
2 - published OSS implementations such as https://github.com/ronnieoverby/AsyncBinaryReaderWriter
3 - the official API when it comes out - https://github.com/dotnet/runtime/issues/17229
