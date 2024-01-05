# Grab Tool

A runtime implementation of Maya's "Grab Tool" in Unity.

## Demo

![Example Scene Demo](./Documentation~/Grab-Tool-Demo.mp4)

## Motivation

I needed a runtime mesh sculpting tool. [Polybrush](https://docs.unity3d.com/Packages/com.unity.polybrush@1.1/manual/index.html) didn't have what I needed.
I instead implemented the basics of [Maya 2024](https://www.autodesk.com/products/maya/overview?term=1-YEAR&tab=subscription)'s _Grab Tool_.

The _Grab Tool_ can be found under the _Sculpting_ tab.

## How to use

See the example scene. It has the functionality for keyboard setup. Remember to turn on "Read/Write Enabled" on the mesh you want to edit.
I included a script that copies the mesh on startup so you only make changes to a temporary mesh (I accidentally changed mine on disk).

## Example Scene Attribution

The mesh was created by myself.

I got the wireframe shaders from https://github.com/Chaser324/unity-wireframe.
