import matplotlib.pyplot as plt
import numpy as np
from matplotlib.backends.backend_pdf import PdfPages
import json
import sys
from pathlib import Path
import os
from datetime import datetime
#sys.path[0] zwraca folder w ktorym jest skrypt, skrypt i json sa w tym samym folderze
with open(sys.path[0]+'//tempData.json') as dataJson:
    data = json.load(dataJson)
    today=datetime.date(datetime.today())
    path=os.path.expanduser('~\Documents\WalkingVisualization\\'+data['animalPrefabName']+'\\'+str(today))
    if not os.path.exists(path):
        os.makedirs(path)
    pp = PdfPages(path+'\\'+str(datetime.now().strftime("%H-%M-%S"))+'.pdf')
    firstPage = plt.figure()
    firstPage.clf()
    firstPage.text(0.5, 0.9, 'Visualization i/o data', fontsize = 16, ha='center')
    txt = 'model name: '+data['animalPrefabName']
    txt += '\npopulation size: '+str(data['populationSize'])
    txt += '\npopulation part size: '+str(data['populationPartSize'])
    txt += '\nmutation rate: '+str(round(data['mutationRate']*100))+'%'
    txt += '\nchase starting position: '+str(round(data['startingPosition'],2))
    txt += '\nspeed: '+str(round(data['speed'],2))
    txt += '\ntime being alive importance: '+str(round(data['timeBeingAliveImportance'],2))
    txt += '\nbest distance: '+str(round(data['bestDistance'],3))
    txt += '\ncurrent generation: '+str(data['currentGen'])
    txt += '\ncurrent best distance: '+str(round(data['currBestDistance'],3))
    firstPage.text(0.1,0.35,txt, transform=firstPage.transFigure, size=12)
    pp.savefig()
    plt.close()
    f=plt.figure()
    plt.plot(data['bestDistances'])
    pp.savefig()
    plt.close()
    #plt.plot(data['bestFitnesses'])

    pp.close()
    dataJson.close()
    os.remove(sys.path[0]+'//tempData.json')
