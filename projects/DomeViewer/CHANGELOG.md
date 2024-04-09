# Changelog
All notable changes to this package will be documented in this file.\
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-04-05
- initial release of Dome Viewer for Windows and Android/Quest

### Known Issues
- NDI streaming currently doesn't support audio input (#9)
- Performance isn't ideal for 4k inputs on Quest 2 (#12)
- When using hand tracking, teleporting is currently disabled (#13)
- When walking around and then switching to Miniature View, the user is positioned too far away from the model (#14)
- When walking around, teleporting, or switching to a smaller Dome layout, the user can leave the dome (#14)