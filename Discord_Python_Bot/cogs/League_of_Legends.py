import discord
from discord.ext import commands
import requests
import bs4 
from bs4 import BeautifulSoup as bs

class League_of_Legends(commands.Cog, name="League of Legends"):
    """Information regarding League of Legends summoners"""


    def __init__(self, client):
        self.client = client
   
    
    @commands.command()
    async def lol(self, ctx, region, *ign):
        """Shows information about the given summoner"""
        headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0'}
        gamename = ""
        for arg in ign:
            gamename = gamename + " " + arg
        message = await ctx.channel.send("Loading")
        if region.lower() == "euw" or region.lower() == "na" or region.lower() == "eune" or region.lower() == "lan" or region.lower() == "kr" or region.lower() == "oce" or region.lower() == "ru" or region.lower() == "tr" or region.lower() == "br" or region.lower() == "jp" or region.lower() == "las":
            website = requests.get("https://"+region.lower()+".op.gg/summoner/userName="+gamename, headers=headers)
            websitestring = "https://"+region.lower()+".op.gg/summoner/userName="+gamename.replace(" ", "+")
            parser = bs4.BeautifulSoup(website.text, 'lxml')
            soup=bs(website.content,"html.parser")            
            image=soup.find("img",class_="ProfileImage")
            try:
                getnaam = parser.select('.Name')
                getupdated = parser.select('.LastUpdate')
                updated = getupdated[0].getText().strip().lower()
                naam = getnaam[0].getText().strip()
                
                try:
                    getsolorank = parser.select('.TierRank')
                    solorank = getsolorank[0].getText().strip()
                    getsololp = parser.select('.LeaguePoints')
                    sololp = getsololp[0].getText().strip().replace(" ", "")
                    getsolowin = parser.select('.wins')
                    solowin = getsolowin[0].getText().strip()
                    getsololose = parser.select('.losses')
                    sololose = getsololose[0].getText().strip()
                    solostats = sololp + "/ " +solowin + " " + sololose
                    getsolowinrate = parser.select('.winratio')
                    solowinrate = getsolowinrate[0].getText().strip()
                    solo = solorank + "\n" + solostats + " - " + solowinrate
                except:
                    solo = "Unranked"
                try:
                    getflexrank = parser.select('.sub-tier__rank-tier')
                    flexrank = getflexrank[0].getText().strip()
                    getflexlp = parser.select('.sub-tier__league-point')
                    flexlp = getflexlp[0].getText().strip()
                    getflexwinrate = parser.select('.sub-tier__gray-text')
                    flexwinrate = getflexwinrate[1].getText().strip()
                    flex = flexrank + "\n" + flexlp + " - " + flexwinrate
                except:
                    flex = "Unranked"            
                
                await message.edit(content="Grabbing completed, preparing message!")
                embed=discord.Embed(title=naam + " - " +region.upper(), color=0x5b7bf9)
                embed.set_author(name=updated, url=websitestring)
                embed.set_thumbnail(url="https:"+image["src"])
                embed.add_field(name="Ranked Solo",value=solo, inline=False)
                embed.add_field(name="Flex 5:5 Rank",value=flex, inline=False)
                embed.set_footer(text="If the data is inaccurate, press the 'Last Updated' and manually update it.")
                await message.delete()
                await ctx.channel.send(embed=embed)
            except:
                await message.edit(content="Something went wrong, User doesn't exist?")
        else:
            await ctx.channel.send("*Error, use command as followed: `.lol Region SummonerName`*")


    @commands.command()
    async def tft(self, ctx, region, *ign):
        """Shows information about the given summoner"""
        headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0'}
        message = await ctx.channel.send("Loading")
        gamename = ""
        for arg in ign:
            gamename = gamename + " " + arg

        if region.lower() == "euw" or region.lower() == "na" or region.lower() == "eune" or region.lower() == "lan" or region.lower() == "kr" or region.lower() == "oce" or region.lower() == "ru" or region.lower() == "tr" or region.lower() == "br" or region.lower() == "jp" or region.lower() == "las":
            website = requests.get("https://lolchess.gg/profile/"+region+"/"+gamename[1:], headers=headers)
            websitesend = str("https://lolchess.gg/profile/"+region+"/"+gamename[1:])
            parser = bs4.BeautifulSoup(website.text, 'lxml')
            soup= bs(website.content,"html.parser")            

            try:
                try:
                    gettftrank = parser.select('.profile__tier__summary__tier')
                    tftrank = gettftrank[0].getText().strip()
                except:
                    tftrank = "N/A"
                try:
                    gettftlp = parser.select('.profile__tier__summary__lp')
                    tftlp = gettftlp[0].getText().strip()
                except:
                    tftlp = ""
                try:
                    gettfttftwins = parser.select('.profile__tier__stat__value')
                    tftwins = gettfttftwins[0].getText().strip()
                except:
                    tftwins = "N/A"
                try:
                    gettftwinrate = parser.select('.profile__tier__stat__value')
                    tftwinrate = gettftwinrate[1].getText().strip()
                except:
                    tftwinrate = "N/A"
                try:
                    gettfttop4 = parser.select('.profile__tier__stat__value')
                    tfttop4 = gettfttop4[2].getText().strip()
                except:
                    tfttop4 = "N/A"
                try:
                    gettfttop4rate = parser.select('.profile__tier__stat__value')
                    tfttop4rate = gettfttop4rate[3].getText().strip()
                except:
                    tfttop4rate = "N/A"
                try:
                    gettftgamesplayed = parser.select('.profile__tier__stat__value')
                    tftgamesplayed = gettftgamesplayed[4].getText().strip()
                except:
                    tftgamesplayed = "N/A"
                try:
                    gettftaverage = parser.select('.profile__tier__stat__value')
                    tftaverage = gettftaverage[5].getText().strip()
                    if tftaverage == "":
                        tftaverage = "N/A"
                except:
                    tftaverage = "N/A"
                try:
                    getupdated = soup.find('div', class_='profile__summoner__status').text.strip()
                except:
                    getupdated = "Needs an update"
                try:
                    getimage=soup.find("div",class_="profile__icon")
                    setimage=getimage.find("img")
                    image= "https:"+setimage["src"]
                except:
                    image = "https://cdna.artstation.com/p/assets/images/images/025/410/380/large/t-j-geisen-tft-iconv2-v005-crop-arstation.jpg?1585702185"
    
        
                embed=discord.Embed(title=gamename + " - " + "TFT", description=tftrank + " " + tftlp, color = ctx.author.color)
                embed.set_author(name=getupdated, url=websitesend)
                embed.set_thumbnail(url=image)
                embed.add_field(name="Win", value=tftwins, inline=True)
                embed.add_field(name="Win rate", value=tftwinrate, inline=True)
                embed.add_field(name="Top4", value=tfttop4, inline=True)
                embed.add_field(name="Top4 Rate", value=tfttop4rate, inline=True)
                embed.add_field(name="Played", value=tftgamesplayed, inline=True)
                embed.add_field(name="Avg. Rank", value=tftaverage, inline=True)
                embed.set_footer(text="If the data is inaccurate, press the <"+getupdated+"> and manually update it.")
    
                await message.delete()
                await ctx.channel.send(embed=embed)
            except:
                await ctx.channel.send("*Error, Try manually updating trough this link, then try again*" )
                await ctx.channel.send("<"+websitesend+">")
        else:
            await ctx.channel.send("*Error, use command as followed: `.TFT Region SummonerName`*")

def setup(client):
    client.add_cog(League_of_Legends(client))