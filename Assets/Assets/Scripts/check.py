import matplotlib.pyplot as plt
import numpy as np
from matplotlib.backends.backend_pdf import PdfPages

l=[1,2,3,4,5,6,4]
l2=[1,1,1,2,5,1,2]
f=plt.figure()
plt.plot(l)
f2=plt.figure()
plt.plot(l2)
pp = PdfPages('foo.pdf')
pp.savefig(f)
pp.savefig(f2)
pp.close()