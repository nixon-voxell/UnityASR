#  Unity Automatic Speech Recognition

> Discalimer: Currently, the AutomaticSpeechRecognition.cs script is not working as intended. For some reasons, it couldn't recognize a single word I say. Please make a pull request if you figured out how to solve this issue!

## What does this package contatins??

This package provides a fully functional cross platform Automatic Speech Recognition using deep learning models intergrated in Unity with C#!!! Anyone can use this package in any way they want as long as they credit the author(s) and also respect the [license](LICENSE) agreement.

### DeepSpeech

[DeepSpeech](https://github.com/mozilla/DeepSpeech) is used as the backend of this package. In order to use DeepSpeech, the source code had to be copied manually as there are some unwanted stuffs in the DLL package.

## Installation

This package depends on:
- [UnityUtil](https://github.com/voxell-tech/UnityUtil)

1. Clone the [UnityUtil](https://github.com/voxell-tech/UnityUtil) repository into your project's `Packages` folder.
2. Download the unitypackage dependencies from [Google Drive](https://drive.google.com/drive/u/5/folders/1WOaWVwdCD9p0oq7S3atoJfLt9V0HND1u) and extract it into Unity.
3. Clone this repository into your project's Packages folder.
4. In Unity, go into `native/DeepSpeechClient`, you can choose either CPU or GPU version of DeepSpeech, GPU version is way faster than CPU inferencing.
5. And you are ready to go!

## Support the project!

<a href="https://www.patreon.com/voxelltech" target="_blank">
  <img src="https://teaprincesschronicles.files.wordpress.com/2020/03/support-me-on-patreon.png" alt="patreon" width="200px" height="56px"/>
</a>

<a href ="https://ko-fi.com/voxelltech" target="_blank">
  <img src="https://uploads-ssl.webflow.com/5c14e387dab576fe667689cf/5cbed8a4cf61eceb26012821_SupportMe_red.png" alt="kofi" width="200px" height="40px"/>
</a>

## Join the community!

<a href ="https://discord.gg/WDBnuNH" target="_blank">
  <img src="https://gist.githubusercontent.com/nixon-voxell/e7ba303906080ffdf65b106f684801b5/raw/65b0338d5f4e82f700d3c9f14ec9fc62f3fd278e/JoinVXDiscord.svg" alt="discord" width="200px" height="200px"/>
</a>


## License

This repository as a whole is licensed under the GNU Public License, Version 3. Individual files may have a different, but compatible license.

See [license file](./LICENSE) for details.