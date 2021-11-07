
public class PStack
{
    string[] strings;
    int count = 0;

    /*
    ==================
    NEW
    ==================
    */
    public PStack(int p)
    {
        strings = new string[p];
        count = 0;
    }

    /*
    ==================
    GetStack
    ==================
    */
    public string[] GetStack()
    {
        return System.ObjectExtensions.Copy(strings);
    }

    /*
    ==================
    GetString
    ==================
    */
    public string GetString(int p)
    {
        return strings[p];
    }

    /*
    ==================
    Count
    ==================
    */
    public int Count()
    {
        return count;
    }

    /*
    ==================
    Add
    ==================
    */
    public void Add(string s)
    {
        if(count>strings.Length-1)
        {
            PushTop(s);
            count = strings.Length;
        }
        else
        {
            strings[count] = s;                    
        }

        count++;
    }

    /*
    ==================
    PushTop
    ==================
    */
    public void PushTop(string s)
    {
        for(int i = 1; i < strings.Length; i++)
        {
            strings[i-1] = strings[i];
        }

        strings[strings.Length-1] = s;
    }

    /*
    ==================
    PushBottom
    ==================
    */
    public void PushBottom(string s)
    {
        for(int i = strings.Length-1; i > 0; i--)
        {
            strings[i] = strings[i-1];
        }

        strings[0] = s;
    }

    /*
    ==================
    Insert
    ==================
    */
    public void Insert(int p, string s)
    {
        if(p >= 0 && p < strings.Length)
        {
            strings[p] = s;
        }            
    }

    /*
    ==================
    Resize
    ==================
    */
    public void Resize(int newSize)
    {
        if(newSize <= 0 || newSize == strings.Length)
        {
            return;
        }

        string[] temp = new string[newSize];
        for(int i = 0; i < newSize; i++)
        {
            if(newSize < strings.Length)
            {      
                // Fill new array in descending order.
                // Newer lines are kept, older lines are discarded.                
                temp[i] = strings[strings.Length - (newSize-i)];
            }
            else
            {
                if (i > count-1)
                {
                    break;
                }
    
                temp[i] = strings[i];                
            }
        }
        
        if(count > newSize)
        {
            count = newSize;
        }

        strings = System.ObjectExtensions.Copy(temp);
    }
    
    /*
    ==================
    Remove
    ==================
    */
    public void Remove(int p)
    {
        for(int i = p; i < strings.Length-1; i ++)
        {
            strings[i] = strings[i+1];
        }

        strings[strings.Length-1] = null;
        count--;
    }
}


