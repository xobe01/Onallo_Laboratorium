import os
import pathlib
import numpy as np
import sys
import tensorflow as tf
import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import keras
from PIL import Image
from keras.models import Model
import cv2

from PIL import Image
from object_detection.utils import ops as utils_ops
from object_detection.utils import label_map_util

if "models" in pathlib.Path.cwd().parts:
  while "models" in pathlib.Path.cwd().parts:
    os.chdir('..')

tf.gfile = tf.io.gfile
returnString = ""

def load_model(model_name):
  model_dir = "/Users/reimholz/neural_network/SonAR_image_generator/Assets/neur/"
  model = tf.saved_model.load(str(model_dir))
  model = model.signatures['serving_default']

  return model

print("szia Boti")
model_name = 'faster_rcnn_inception_v2_coco_2018_01_28'
detection_model = load_model(model_name)

def run_inference_for_single_image(model, image):
  image = np.asarray(image)
  input_tensor = tf.convert_to_tensor(image)
  input_tensor = input_tensor[tf.newaxis,...]
  output_dict = model(input_tensor)
  num_detections = int(output_dict.pop('num_detections'))
  output_dict = {key:value[0, :num_detections].numpy() 
                 for key,value in output_dict.items()}
   
  return output_dict

def get_bounding_box(model, image_path):
  image_np = np.array(Image.open(image_path))
  output_dict = run_inference_for_single_image(model, image_np)
  height,width = image_np.shape[:2]
  ymin = int(output_dict['detection_boxes'][0][0]*height)
  xmin = int(output_dict['detection_boxes'][0][1]*width)
  ymax = int(output_dict['detection_boxes'][0][2]*height)
  xmax = int(output_dict['detection_boxes'][0][3]*width)
  boundaries = str(xmin) +';'+ str(xmax) +';'+ str(ymin) +';'+ str(ymax) +';'
  return image_np[ymin:ymax,xmin:xmax],boundaries

image_path = '/Users/reimholz/neural_network/SonAR_image_generator/Assets/neur/'+ sys.argv[1]
cropped_image,returnString = get_bounding_box(detection_model, image_path)

def preprocess(im):
    
    im = im/255
    im -= .5
    return im

channel = 1
im_size = 100

model_xyz = keras.models.load_model('/Users/reimholz/neural_network/SonAR_image_generator/Assets/neur/xyzRotModel')

def predict(image):

    im = Image.fromarray(image, 'RGB')
    im = im.convert('L')
    im = im.resize((im_size,im_size), Image.ANTIALIAS)
    im = np.array(im)
    im = preprocess(im).reshape((1, im_size, im_size, channel))

    rot = model_xyz.predict(im)

    xRot = int(rot[0]*150)
    yRot = int(rot[1]*150)
    zRot = int(rot[2]*360)

    return str(xRot-75) + ';' + str(yRot-75) + ';' + str(zRot) + ';'

returnString += predict(cropped_image)
print(returnString)