import os
import pathlib
import numpy as np
import keras
from PIL import Image
from keras.models import Model
import socket
import threading
from io import BytesIO
import tensorflow as tf
import time
from PIL import ImageFile
print('imported')

ImageFile.LOAD_TRUNCATED_IMAGES = True
conn = None
recv_data = []
im_size_first = 300,300
channel = 1
im_size_second = 100
TCP_IP = '192.168.0.18'
TCP_PORT = 8888
BUFFER_SIZE = 200000

def image_listener():
    
    global conn
    global recv_data
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((TCP_IP, TCP_PORT))
        s.listen()
        conn, addr = s.accept()
        with conn:
            while True:
                data = conn.recv(BUFFER_SIZE)
                recv_data.insert(0, data)
                if(len(recv_data)==10):
                    recv_data.pop()
 
            s.close() 

def run_inference_for_single_image(model, image):
  input_tensor = tf.convert_to_tensor(image)
  input_tensor = input_tensor[tf.newaxis,...]  
  output_dict = model(input_tensor)
  num_detections = int(output_dict.pop('num_detections'))
  output_dict = {key:value[0, :num_detections].numpy() 
                 for key,value in output_dict.items()}
   
  return output_dict

def get_bounding_box(model, im):
  im_resized = im.resize(im_size_first, Image.ANTIALIAS)
  image_np = np.array(im_resized)  
  output_dict = run_inference_for_single_image(model, image_np)  
  
  if(len(output_dict) > 0):
    if(output_dict['detection_scores'][0]>0.7):
    
      height,width = image_np.shape[:2]
      ymin = int(output_dict['detection_boxes'][0][0]*height)
      xmin = int(output_dict['detection_boxes'][0][1]*width)
      ymax = int(output_dict['detection_boxes'][0][2]*height)
      xmax = int(output_dict['detection_boxes'][0][3]*width)
      yPlusSize = 0#int((ymax-ymin)*0.2)
      xPlusSize = 0#int((xmax-xmin)*0.2)
      boundaries = str(xmin) +';'+ str(xmax) +';'+ str(ymin) +';'+ str(ymax) +';'
      return image_np[ymin-yPlusSize:ymax+yPlusSize,xmin-xPlusSize:xmax+xPlusSize],boundaries
      
  return None, 'null'

def preprocess(im):
    
    im = im/255
    im -= .5
    return im

def predict(image):

    im = Image.fromarray(image, 'RGB')
    im = im.convert('L')
    im = im.resize((im_size_second,im_size_second), Image.ANTIALIAS)
    im = np.array(im)
    im = preprocess(im).reshape((1, im_size_second, im_size_second, channel))

    rot = model_xyz.predict(im)

    xRot = int(rot[0]*90)
    yRot = int(rot[1]*90)
    zRot = int(rot[2]*360)

    return str(xRot-45) + ';' + str(yRot-45) + ';' + str(zRot) + ';'
    
def run_neural_net():
    global conn
    global recv_data
    while True:
        if(len(recv_data)>0):
            stream = BytesIO(recv_data[0])
            im = Image.open(stream).convert("RGB")
            cropped_image,returnString = get_bounding_box(detection_model, im)
            if(returnString != 'null'):
                returnString += predict(cropped_image)
                sendData = bytes(returnString, 'utf-8')
                conn.send(sendData)
            print(returnString)
        else:
                time.sleep(0.1)
                    
image_listener_thread = threading.Thread(target=image_listener)
neural_net_thread = threading.Thread(target=run_neural_net)

detection_model = tf.saved_model.load('C:/Users/ungbo/neural_network/SonAR_image_generator/Assets/neur/saved_model')
model_xyz = keras.models.load_model('/Users/ungbo/neural_network/SonAR_image_generator/Assets/neur/xyzRotModel')

image_listener_thread.start()
neural_net_thread.start()
print('ready')


    