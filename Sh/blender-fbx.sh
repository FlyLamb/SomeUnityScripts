#!/bin/sh

#    Simple script that allows you to use blender-fbx as the open application for FBX files, using it's import capabilities
#    Written by bajtixone (https://github.com/Bajtix) (https://bajtix.xyz/)

#    If you decide to use this script feel free to do so, I don't require credit, but if it's gonna be open source it'd be sick if you were to keep this comment.




blender --python-expr "import bpy; bpy.context.preferences.view.show_splash = False; bpy.ops.object.select_all(action = 'SELECT'); bpy.ops.object.delete(); bpy.ops.import_scene.fbx( filepath = '$1' ); bpy.ops.object.select_all(action = 'DESELECT')"
