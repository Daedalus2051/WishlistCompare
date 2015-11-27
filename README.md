# WishlistCompare
A project to pull items from a Steam wishlist and get price history for the lowest regular and sale price for each game (yup I'm that bored on Black Friday...).

My ultimate goal with this project is simply to use it as a means of learning... the biggest problem with the concept is that the way it's set up is by design "unmaintainable". By this I mean that the data I'm collecting is from pulling down the HTML of a website and using the HTML Agility pack to parse it so I can get the data I need. If the website (be it Steam or steamprices) changes the way they format their HTML then that means that the project needs to be re-written to handle that.

But for now it's something to do and it's intended to be a learning experience into WPF (data binding and control formatting) as well as an interesting look at Xpath, because it's practically a requirement to parse the HTML (using the agility pack).

FYI for those who don't want to look through the code, you can find the HTML Agility pack here: https://htmlagilitypack.codeplex.com/
