Counter communication is materialized via a named pipe.

First byte is a command.
If an operation has a payload it is in the second byte.
For the status the payload byte means: 0 = off, 1 = on.

The operations are:
- get status
  - request payload is 0 bytes
  - response payload is 1 byte
- set status
  - request payload is 1 byte
  - response is not returned
