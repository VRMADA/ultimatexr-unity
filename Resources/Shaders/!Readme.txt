These shaders are placed in /Resources so that they are forced to be included in the build.
This enables creating materials at runtime through scripting making sure that shaders will be available even if they are not referenced in any scene:
material = new Material(Shader.Find(shaderName));