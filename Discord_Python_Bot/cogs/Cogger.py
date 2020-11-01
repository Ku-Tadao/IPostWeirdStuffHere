from discord.ext import commands

class Cogger(commands.Cog):

    def __init__(self, client):
        self.client = client

    @commands.command(hidden=True)
    @commands.is_owner()
    async def reload(self, ctx, *, module : str):
        self.client.reload_extension("cogs."+module) 
        await ctx.send("Reloaded")

    @commands.command(hidden=True)
    @commands.is_owner()
    async def load(self, ctx, *, module : str):
        self.client.load_extension("cogs."+module) 
        await ctx.send("Loaded")

    @commands.command(hidden=True)
    @commands.is_owner()
    async def unload(self, ctx, *, module : str):
        self.client.unload_extension("cogs."+module) 
        await ctx.send("Unloaded")


def setup(client):
    client.add_cog(Cogger(client))