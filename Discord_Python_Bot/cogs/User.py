import discord
from discord.ext import commands

class User(commands.Cog):
    def __init__(self, client):
        self.client = client
   

    @commands.command()
    async def avatar(self, ctx, *, avamember):
        if "<@!" in avamember:
            getusr:discord.user = await self.client.fetch_user(avamember[3:-1])
            userAvatarUrl = getusr.avatar_url
            await ctx.send("`"+str(getusr)+"`'s avatar is: "+str(userAvatarUrl))
        else:
            getusr:discord.user = await self.client.fetch_user(avamember)
            userAvatarUrl = getusr.avatar_url
            await ctx.send("`"+str(getusr)+"`'s avatar is: "+str(userAvatarUrl))

    
def setup(client):
    client.add_cog(User(client))
   