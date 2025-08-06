﻿using LanguageExt;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Community.People.Person;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryPersonWrite
{
    void Add(Person person);

    void Update(Person person);
}