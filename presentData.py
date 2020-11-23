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
    generationBestDistance=data['bestDistances'].index(max(data['bestDistances']))
    generationBestFitness=data['bestFitnesses'].index(max(data['bestFitnesses']))
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
    txt += '\ntime below average penalty: '+str(round(data['timeBelowAveragePenalty'],2))
    txt += '\nbest fitness value: '+str(round(data['bestFitness'],3))
    txt += '\ngeneration with best fitness value: '+str(generationBestDistance)
    txt += '\nbest distance: '+str(round(data['bestDistance'],3))
    txt += '\ngeneration with best distance: '+str(generationBestDistance)
    txt += '\ncurrent generation: '+str(data['currentGen'])
    txt += '\ncurrent best distance: '+str(round(data['currBestDistance'],3))
    txt += '\ncurrent best fitness value: '+str(round(data['currBestFitness'],3))
    firstPage.text(0.1,0.25,txt, transform=firstPage.transFigure, size=12)
    pp.savefig()
    plt.close()

    f, ax = plt.subplots(1,1)
    plt.plot([generationBestDistance,generationBestDistance],[data['bestDistance'],min(data['bestDistances'])],'--k',linewidth=0.5)
    plt.plot([0,generationBestDistance],[data['bestDistance'],data['bestDistance']],'--k',linewidth=0.5)
    plt.rc_context({'axes.autolimit_mode': 'round_numbers'})
    plt.plot(data['bestDistances'])
    x1,x2,y1,y2 = plt.axis()
    xt = ax.get_xticks() 
    xt=np.append(xt,generationBestDistance)
    xtl=xt.tolist()
    xtl[-1]=generationBestDistance
    ax.set_xticks(xt)
    ax.set_xticklabels(xtl)

    yt = ax.get_yticks() 
    yt=np.append(yt,round(data['bestDistance'],3))
    ytl=yt.tolist()
    ytl[-1]=round(data['bestDistance'],3)
    ax.set_yticks(yt)
    ax.set_yticklabels(ytl)
    
    plt.axis((-1,x2,y1,y2))

    

    plt.title("Best distance in individual generations")
    plt.xlabel("generation number")
    plt.ylabel("best distance")
    pp.savefig()
    plt.close()

    f, ax = plt.subplots(1,1)
    plt.plot([generationBestFitness,generationBestFitness],[data['bestFitness'],min(data['bestFitnesses'])],'--k',linewidth=0.5)
    plt.plot([0,generationBestFitness],[data['bestFitness'],data['bestFitness']],'--k',linewidth=0.5)
    plt.rc_context({'axes.autolimit_mode': 'round_numbers'})
    plt.plot(data['bestFitnesses'])
    x1,x2,y1,y2 = plt.axis()
    xt = ax.get_xticks() 
    xt=np.append(xt,generationBestFitness)
    xtl=xt.tolist()
    xtl[-1]=generationBestFitness
    ax.set_xticks(xt)
    ax.set_xticklabels(xtl)

    yt = ax.get_yticks() 
    yt=np.append(yt,round(data['bestFitness'],3))
    ytl=yt.tolist()
    ytl[-1]=round(data['bestFitness'],3)
    ax.set_yticks(yt)
    ax.set_yticklabels(ytl)
    
    plt.axis((-1,x2,y1,y2))

    

    plt.title("Best fitness value in individual generations")
    plt.xlabel("generation number")
    plt.ylabel("best fitness value")
    pp.savefig()
    plt.close()
    #plt.plot(data['bestFitnesses'])

    pp.close()
    dataJson.close()
    #os.remove(sys.path[0]+'//tempData.json')

