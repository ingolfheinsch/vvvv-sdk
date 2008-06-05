#ifndef __GUI_H
#define __GUI_H

#include <stdio.h>
#include <windows.h>
#include "pluginterfaces/vst2.x/aeffectx.h"
#include "public.sdk\source\vst2.x\aeffeditor.h"
#include "VSTPlugin.h"


#define _CRT_SECURE_NO_DEPRECATE
#define WNDWIDTH       320
#define WNDHEIGHT      130
#define MAXEDITORCOUNT 2


LRESULT CALLBACK WndProc( HWND, UINT, WPARAM, LPARAM);

struct Editor
{
  AEffect *effect;
  int     *width;
  int     *height;
  HWND    *hwnd;

};

struct EDITORWINDOW : DLGTEMPLATE
{
  WORD ext[3];
  EDITORWINDOW () { memset( this, 0, sizeof(*this) ); }

};

INT_PTR CALLBACK editorCallback ( HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

DWORD WINAPI windowThread (LPVOID data);


#endif