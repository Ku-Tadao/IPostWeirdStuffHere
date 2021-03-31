import discord
from discord.ext import commands
import requests
import bs4 
from bs4 import BeautifulSoup as bs

class NetworkCalc(commands.Cog, name="\nIP / Subnet Calculator"):
    """\nIP / Subnet mask calculator"""


    def __init__(self, client):
        self.client = client
   
    
    @commands.command()
    async def ncalc(self, ctx, adress, netmask):
        netmask = int(netmask)
        if netmask<= 32:
            message = await ctx.channel.send("Loading")

            headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0'}
            website = requests.get("http://jodies.de/ipcalc?host="+adress+"&mask1="+str(netmask)+"&mask2=", headers=headers)
            soup=bs(website.content,"html.parser")            
            gettexts=soup.findAll("font")
            getdoublefonts=gettexts[13].findAll("font")


            await message.edit(content="""```markdown\n""""#"+gettexts[0].text +"\n "+gettexts[1].text+"\n "+gettexts[2].text+"\n> ----------------------------------------- \n#"+gettexts[3].text+" \n "+gettexts[4].text+" \n "+gettexts[5].text+"\n> ----------------------------------------- \n#"+gettexts[9].text+" \n "+gettexts[10].text+" \n "+gettexts[11].text+gettexts[12].text[:-1] +"\n> ----------------------------------------- \n#"+getdoublefonts[1].text+"\n "+getdoublefonts[2].text +"\n "+getdoublefonts[3].text+"\n> ----------------------------------------- \n#"+getdoublefonts[4].text+" \n "+getdoublefonts[5].text+" \n "+getdoublefonts[6].text+"\n> ----------------------------------------- \n#"+getdoublefonts[7].text+" \n "+getdoublefonts[8].text+"\n "+getdoublefonts[9].text+"\n> ----------------------------------------- \n#"+getdoublefonts[10].text+" \n "+getdoublefonts[11].text+" \n```")
        else:
            await ctx.channel.send("*Error, use command as followed: `.ncalc ipadress subnet`*")        

def setup(client):
    client.add_cog(NetworkCalc(client))