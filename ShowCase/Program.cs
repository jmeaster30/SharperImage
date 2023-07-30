﻿// See https://aka.ms/new-console-template for more information

using SharperImage.Formats;
using SharperImage.Viewer;

using var imageFile = File.OpenRead("../../../images/qoi/dice.qoi");
var image = QoiImage.LoadImage(imageFile);
Viewer.Open(image);
