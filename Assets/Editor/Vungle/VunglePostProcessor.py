#!/usr/bin/python

import sys
from mod_pbxproj import XcodeProject

pathToProjectFile = sys.argv[1] + '/Unity-iPhone.xcodeproj/project.pbxproj'
pathToNativeCodeFiles = sys.argv[2]

project = XcodeProject.Load( pathToProjectFile )

for obj in list(project.objects.values()):
    if 'path' in obj:
        if project.path_leaf( 'System/Library/Frameworks/AdSupport.framework' ) == project.path_leaf(obj.get('path')):
            project.remove_file(obj.id)

if project.modified:
    project.save()

project.add_folder( pathToNativeCodeFiles, excludes=["^.*\.meta$"] )

project.add_file_if_doesnt_exist( 'System/Library/Frameworks/AdSupport.framework', tree='SDKROOT', weak=True, parent='Frameworks' )
project.add_file_if_doesnt_exist( 'System/Library/Frameworks/StoreKit.framework', tree='SDKROOT', parent='Frameworks' )
project.add_file_if_doesnt_exist( 'System/Library/Frameworks/WebKit.framework', tree='SDKROOT', parent='Frameworks' )
project.add_file_if_doesnt_exist( 'usr/lib/libsqlite3.dylib', tree='SDKROOT', parent='Frameworks' )
project.add_file_if_doesnt_exist( 'usr/lib/libz.1.1.3.dylib', tree='SDKROOT', parent='Frameworks' )

project.add_other_ldflags(['-ObjC'])

if project.modified:
    project.save()

